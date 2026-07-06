using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps.Serializer {
    internal class BasicStringReader {
        private readonly string _str;
        private int _position;

        public BasicStringReader(Type type, string str) {
            _str = str;
            if (ReadTo(':') != type.Name) {
                throw new ArgumentException(
                    $"Invalid type. Expected: {type.Name} in: '{str}'");
            }
            ThrowOrMove(':');
            ThrowOrMove('{');
        }

        private void ThrowOrMove(params char[] v) {
            ThrowIfAtTheEnd();
            if (Array.IndexOf(v, _str[_position]) == -1) {
                throw new ArgumentException(
                    $"Invalid character at {_position}. " +
                    $"Expected any of '{string.Join(",", v)}', " +
                    $"actual '{_str[_position]}' " +
                    $"in: '{_str}'");
            }
            _position++;
        }

        private void ThrowIfAtTheEnd() {
            if (_position >= _str.Length) {
                throw new ArgumentException("Reached end of string" +
                    $" at {_position} in: '{_str}'");
            }
        }

        private string ReadTo(char v) {
            var start = _position;
            var pos = IndexOf(v);
            if (pos == -1) {
                throw new ArgumentException(
                    $"Couldn't find '{v}' after {_position} in: '{_str}'");
            }
            _position = pos;
            ThrowIfAtTheEnd();
            return _str.Substring(start, _position - start);
        }

        private int IndexOf(char v, int maxPosition = -1) {
            if (maxPosition == -1) {
                maxPosition = _str.Length - 1;
            }
            var pos = _position;
            var braceCount = 0;
            while (pos <= maxPosition) {
                if (_str[pos] == '{') {
                    braceCount++;
                } else if (braceCount > 0 && _str[pos] == '}') {
                    braceCount--;
                } else if (braceCount == 0 && _str[pos] == v) {
                    break;
                }
                pos++;
            }
            if (pos > maxPosition) {
                return -1;
            }
            return pos;
        }

        public bool FinishReading() {
            if (_position < _str.Length) {
                if (_str[_position] != '}') {
                    throw new ArgumentException(
                        $"Not finished reading data. Expected: '}}', actual '{_str[_position]}' in: '{_str}'");
                }
                _position++;
            }
            return _position == _str.Length;
        }

        internal string ReadValue() {
            var pos = IndexOf(';');
            if (pos == -1) {
                pos = IndexOf('}');
                if (pos == -1) {
                    throw new ArgumentException(
                        $"Couldn't find end of value after {_position} " +
                        $"in: '{_str}'");
                }
            }
            var value = _str.Substring(_position, pos - _position);
            _position = pos + 1;
            return value;
        }

        internal IEnumerable<string> ReadEnumerable() {
            if (_str[_position] == ';') {
                // empty enumerable
                _position++;
                yield break;

            }
            if (_str[_position] == '}') {
                // empty enumerable at the end of the object
                yield break;

            }
            ThrowOrMove('[');
            var end = IndexOf(']');
            if (end == -1) {
                throw new ArgumentException(
                    $"Couldn't find end of enumerable starting at " +
                    $"{_position - 1} in: '{_str}'");
            } else if (end == _position) {
                // empty enumerable
                _position++;
                ThrowOrMove(';', '}');
                yield break;
            }
            while (_position < end) {
                var valueEnd = IndexOf(',', end); // [a,b,c], _position = 5, end = 6, end - _position = 1, expected = 1, valueEnd = 6
                if (valueEnd == -1) {
                    valueEnd = end;
                }
                yield return _str.Substring(_position, valueEnd - _position);
                _position = valueEnd + 1;
            }
            ThrowOrMove(';', '}');
        }
    }

}
