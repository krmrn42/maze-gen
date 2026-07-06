using System;
using System.Linq;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Serializer {
    public class CellSerializer : IStringSerializer<Cell> {
        private readonly AreaType _defaultType;

        public CellSerializer(AreaType defaultType) {
            _defaultType = defaultType;
        }

        public Cell Deserialize(string str) {
            var serializer = new BasicStringReader(typeof(Cell), str);
            var type = _defaultType;
            var typeStr = serializer.ReadValue();
            if (typeStr != "") {
                type = (AreaType)Enum.Parse(typeof(AreaType), typeStr);
            }
            var cell = new Cell(type);
            foreach (var link in serializer.ReadEnumerable()) {
                cell.HardLinks.Add(Vector.Parse(link));
            }
            foreach (var tag in serializer.ReadEnumerable()) {
                cell.Tags.Add(new Cell.CellTag(tag));
            }
            return cell;
        }

        /// <summary>
        /// Serializes the specified cell into a string of the form:
        /// <c>{POSITION;[LINK,[LINK,...]];[TAG,[TAG,...]]}</c>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string Serialize(Cell obj) {
            return new BasicStringWriter()
                .WriteObjectStart(obj.GetType())
                .WriteValue(obj.AreaType == _defaultType ? "" : obj.AreaType.ToString())
                .WriteEnumerable(obj.HardLinks.Select(v => v.ToString()))
                .WriteEnumerable(obj.Tags.Select(v => v.ToString()))
                .WriteObjectEnd();
        }
    }

}
