

using System;
using System.Collections.Generic;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents an extensible object that can store and retrieve values of any
    /// type.
    /// </summary>
    public class ExtensibleObject {
        private readonly Dictionary<Type, object> _x =
            new Dictionary<Type, object>();

        /// <summary>
        /// Adds a value of a specific type to the extensible object.
        /// </summary>
        /// <typeparam name="T">The type of the value to add.</typeparam>
        /// <param name="value">The value to add. It must be of type T or a type
        /// that can be assigned to T.</param>
        /// <exception cref="ArgumentException">Thrown if a value of the same type
        /// has already been added.</exception>
        public void X<T>(T value) {
            if (_x.ContainsKey(typeof(T)))
                _x[typeof(T)] = value;
            else _x.Add(typeof(T), value);
        }

        /// <summary>
        /// Retrieves a value of a specific type from the extensible object.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <returns>The value of type T stored in the extensible object.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no value of type T is
        /// found.</exception>
        public T X<T>() {
            return _x.ContainsKey(typeof(T)) ? (T)_x[typeof(T)] : default;
        }
    }
}
