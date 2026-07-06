using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlayersWorlds.Maps.Serializer {
    internal class BasicStringWriter {
        private readonly StringBuilder _buffer;
        private readonly TextWriter _writer;
        private bool _first = true;

        public BasicStringWriter() {
            _buffer = new StringBuilder();
            _writer = new StringWriter(_buffer);
        }

        public BasicStringWriter WriteObjectStart(Type t) {
            _writer.Write(t.Name);
            _writer.Write(":");
            _writer.Write("{");
            return this;
        }

        public string WriteObjectEnd() {
            _writer.Write("}");
            _writer.Flush();
            return _buffer.ToString();
        }

        public BasicStringWriter WriteValue(string value) {
            if (_first) {
                _first = false;
            } else {
                _writer.Write(";");
            }
            _writer.Write(value);
            return this;
        }

        public BasicStringWriter WriteEnumerableIf(IEnumerable<string> value, bool condition) {
            if (condition) {
                WriteEnumerable(value);
            } else {
                if (_first) {
                    _first = false;
                } else {
                    _writer.Write(";");
                }
            }
            return this;
        }

        public BasicStringWriter WriteEnumerable(IEnumerable<string> value) {
            if (_first) {
                _first = false;
            } else {
                _writer.Write(";");
            }
            var written = 0;
            foreach (var v in value) {
                _writer.Write(written == 0 ? "[" : ",");
                _writer.Write(v);
                written++;
            }
            if (written > 0) {
                _writer.Write("]");
            }
            return this;
        }
    }

}
