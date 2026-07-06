namespace PlayersWorlds.Maps.Serializer {
    /// <summary>
    /// A contract for classes that can serialize and deserialize objects of 
    /// type <typeparamref name="T" /> to and from strings.
    /// </summary>
    /// <typeparam name="T">The object type to serialize and deserialize.
    /// </typeparam>
    public interface IStringSerializer<T> {

        /// <summary>
        /// Serializes the specified object of type <typeparamref name="T" />
        /// into a string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A string representation of the object.</returns>
        string Serialize(T obj);

        /// <summary>
        /// Deserializes the specified string into an object of type
        /// <typeparamref name="T" />.
        /// </summary>
        /// <param name="str">The string to deserialize.</param>
        /// <returns>An object of type <typeparamref name="T" />.</returns>
        T Deserialize(string str);
    }

}
