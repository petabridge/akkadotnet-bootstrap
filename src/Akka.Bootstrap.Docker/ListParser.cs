using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Akka.Configuration;

namespace Akka.Bootstrap.Docker
{
    public enum TokenizerState
    {
        Start,
        EndOfString,
        AfterComma,
        String,
        Quotes,
    }

    public class ListParser
    {
        private const char NullChar = (char) 0;

        private char[] _buffer;
        private int _position;
        private readonly List<string> _tokens = new List<string>();
        private TokenizerState _state = TokenizerState.Start;
        private readonly StringBuilder _tokenBuilder = new StringBuilder();

        public IEnumerable<string> Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return _tokens;

            _buffer = value.ToArray();
            ParseToken();
            return _tokens;
        }

        [DebuggerStepThrough]
        private void Consume()
        {
            if (_position < _buffer.Length)
                _position++;
        }

        [DebuggerStepThrough]
        private void ConsumeConsecutiveWhiteSpaces()
        {
            while (_position < _buffer.Length && char.IsWhiteSpace(_buffer[_position]))
            {
                Consume();
            }
        }

        [DebuggerStepThrough]
        private char Peek(out bool eol)
        {
            if (_position >= _buffer.Length)
            {
                eol = true;
                return NullChar;
            }

            eol = false;
            return _buffer[_position];
        }

        private void ParseToken()
        {
            // trim leading whitespaces
            ConsumeConsecutiveWhiteSpaces();
            
            while (true)
            {
                var peeked = Peek(out var eol);
                if (eol)
                    break;

                switch (peeked)
                {
                    // String starts with a quote
                    case var c when c == '"':
                    {
                        if (_state == TokenizerState.Start || _state == TokenizerState.AfterComma)
                        {
                            // first case, getting in and out of quote state
                            ParseQuote();
                            break;
                        }
                        // second case, illegal quote
                        throw new ConfigurationException($"Quote SHOULD NOT appear in the middle of a string. Position: [{_position}]");
                    }

                    // comma
                    case var c when c == ',':
                    {
                        // Illegal comma prefixes
                        if (_state == TokenizerState.Start)
                            throw new ConfigurationException(
                                $"Environment variable value SHOULD NOT start with a comma. Position: [{_position}]");

                        if (_state == TokenizerState.AfterComma)
                            throw new ConfigurationException(
                                $"A list SHOULD NOT contain empty string. Illegal comma after comma. Position: [{_position}]");

                        if (_state != TokenizerState.EndOfString)
                            throw new ConfigurationException(
                                $"Unknown environment variable state while parsing position [{_position}]");

                        _state = TokenizerState.AfterComma;
                        Consume();
                        ConsumeConsecutiveWhiteSpaces();
                        break;
                    }

                    // Opening square brackets only accepted at the start of the value
                    case var c when c == '[':
                    {
                        if (_state != TokenizerState.Start)
                            throw new ConfigurationException(
                                $"Opening square brackets can only appear once at the start of the environment variable value. Position: [{_position}]");
                        // Use AfterComma to trap illegal opening square brackets
                        _state = TokenizerState.AfterComma;
                        // consume it and go on
                        Consume();
                        ConsumeConsecutiveWhiteSpaces();
                        break;
                    }

                    // Closing square brackets can only appear at the end of the value
                    // One trailing comma is accepted
                    case var c when c == ']':
                    {
                        var bracketPosition = _position;
                        Consume();
                        ConsumeConsecutiveWhiteSpaces();
                        Peek(out eol);
                        if (!eol || (_state != TokenizerState.AfterComma && _state != TokenizerState.EndOfString))
                            throw new ConfigurationException(
                                $"Closing square brackets can only appear once at the end of the environment variable value. Position: [{bracketPosition}]");
                        break;
                    }

                    // Anything else is a start of an unquoted string
                    case var c:
                        ParseString();
                        break;
                }
            }
        }

        private void ParseString()
        {
            if (_state != TokenizerState.Start && _state != TokenizerState.AfterComma)
            {
                throw new ConfigurationException(
                    $"Unknown parse error. Invalid character at position: [{_position}]");
            }

            var startPosition = _position;
            _state = TokenizerState.String;
            while (_state == TokenizerState.String)
            {
                var peeked = Peek(out var eol);
                if (eol)
                {
                    _state = TokenizerState.EndOfString;
                    break;
                }

                switch (peeked)
                {
                    // Invalid characters
                    case '"':
                        throw new ConfigurationException($"Quote SHOULD NOT appear in the middle of a string. Position: [{_position}]");

                    // second case, escaped character, consume anything after it
                    // this consumes quotes if they appear after it
                    case var c when c == '\\':
                    {
                        Consume();
                        var secondChar = Peek(out eol);
                        if (eol)
                            throw new ConfigurationException($"Invalid escape character. Position: [{_position}]");

                        _tokenBuilder.Append($"\\{secondChar}");
                        break;
                    }

                    // third case, comma, end of string
                    case var c when c == ',':
                    {
                        _state = TokenizerState.EndOfString;
                        break;
                    }

                    // fourth case, closing square brackets, end of string
                    case var c when c == ']':
                    {
                        _state = TokenizerState.EndOfString;
                        break;
                    }

                    // anything else get shoved into the token buffer
                    case var c:
                        Consume();
                        _tokenBuilder.Append(c);
                        break;
                }

            }

            var result = _tokenBuilder.ToString().Trim();
            if (string.IsNullOrWhiteSpace(result))
                throw new ConfigurationException(
                    $"A list SHOULD NOT contain empty string. String start: [{startPosition}], end: [{_position}]");
            _tokens.Add(result);
            _tokenBuilder.Clear();

            ConsumeConsecutiveWhiteSpaces();
        }

        private void ParseQuote()
        {
            if (_state != TokenizerState.Start && _state != TokenizerState.AfterComma)
            {
                throw new ConfigurationException(
                    $"Invalid environment variable value. Quotes SHOULD NOT appear in the middle of a string.  Position: [{_position}]");
            }

            var startPosition = _position;
            Consume();
            _state = TokenizerState.Quotes;

            var eol = false;
            while (!eol && _state == TokenizerState.Quotes)
            {
                switch (Peek(out eol))
                {
                    // first case, exit parse quote state
                    case var c when c == '"':
                    {
                        Consume();
                        var result = _tokenBuilder.ToString();
                        if(string.IsNullOrWhiteSpace(result))
                            throw new ConfigurationException(
                                $"A list SHOULD NOT contain empty string. String start: [{startPosition}], end: [{_position}]");
                        _tokens.Add(result);
                        _tokenBuilder.Clear();
                        _state = TokenizerState.EndOfString;
                        break;
                    }

                    // second case, escaped character, consume anything after it
                    // this consumes quotes if they appear after it
                    case var c when c == '\\':
                        Consume();
                        var secondChar = Peek(out eol);
                        if (eol)
                            throw new ConfigurationException($"Invalid escape character. Position: [{_position}]");

                        _tokenBuilder.Append($"\\{secondChar}");
                        break;

                    // put anything else into the token
                    case var c:
                        Consume();
                        _tokenBuilder.Append(c);
                        break;
                }
            }

            if (_state != TokenizerState.EndOfString)
                throw new ConfigurationException($"Invalid environment variable value. Quoted string SHOULD be terminated with a quote. String start: [{startPosition}], end: [{_position}]");

            ConsumeConsecutiveWhiteSpaces();
        }
    }
}
