using System;

namespace PlayersWorlds.Maps {
    /// <summary>
    /// Represents a container object which may or may not contain a non-null value.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the Optional.</typeparam>
    // TODO: See if we can get rid of this class by leveraging the conventional
    //       TryGetValue pattern.
    public class Optional<T>
    where T : class {

        /// <summary>
        /// Indicates whether the Optional contains a value.
        /// </summary>
        public bool HasValue { get; private set; }

        private readonly T _value;

        /// <summary>
        /// Creates a new Optional&lt;T&gt; instance with the given value.
        /// </summary>
        public Optional(T value) {
            _value = value;
            if (_value != null) {
                HasValue = true;
            }
        }

        /// <summary>
        /// Creates a new empty Optional&lt;T&gt; instance.
        /// </summary>
        public Optional() {
            _value = null;
        }

        /// <summary>
        /// A new empty Optional instance.
        /// </summary>
        public static Optional<T> Empty => new Optional<T>();

        /// <summary>
        /// Returns the value if present, otherwise throws an
        /// InvalidOperationException.
        /// </summary>
        /// <returns>The value if present.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the Optional is empty.</exception>
        public T Value {
            get {
                if (!HasValue) {
                    throw new InvalidOperationException(
                        $"This {this.GetType().Name} doesn't have value.");
                }
                return _value;
            }
        }

        /// <summary>
        /// Checks the equality between the current Optional&lt;T&gt; or its
        /// value with the other object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj is null) {
                return this.HasValue == false;
            }
            if (obj is T) {
                return this.HasValue && this.Value.Equals(obj);
            }
            var other = obj as Optional<T>;
            if (other == null) {
                return false;
            } else {
                return this.HasValue && other.HasValue &&
                       this.Value.Equals(other.Value);
            }
        }

        /// <summary>
        /// If this optional has a value, returns the hash code of its value,
        /// otherwise returns the hash code of the empty Optional&lt;T&gt;.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            if (HasValue) {
                return _value.GetHashCode();
            }
            return base.GetHashCode();
        }

        /// <summary>
        /// A brief string representation of the current Optional&lt;T&gt;.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Optional<{typeof(T).Name}>" +
                $"({(HasValue ? _value.ToString() : "<empty>")})";
        }

        /// <summary>
        /// Allows explicit conversion of an Optional&lt;T&gt; to its value.
        /// </summary>
        /// <param name="optional"></param>
        public static explicit operator T(Optional<T> optional) =>
            optional.HasValue ? optional.Value :
            throw new InvalidOperationException($"This Optional<{typeof(T).Name}> is empty");

        /// <summary>
        /// Delegates the <c>==</c> operator to <see cref="Equals(object)"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Optional<T> left, Optional<T> right) {
            if (left is null) {
                if (right is null) {
                    return Object.ReferenceEquals(left, right);
                } else {
                    return right.Equals(left);
                }
            } else {
                return left.Equals(right);
            }
        }

        /// <summary>
        /// Delegates the <c>!=</c> operator to <see cref="Equals(object)"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Optional<T> left, Optional<T> right) {
            if (left is null) {
                return !right?.Equals(left) ?? false;
            } else {
                return !left.Equals(right);
            }
        }

        /// <summary>
        /// Allows implicit conversion of an object of type T to
        /// Optional&lt;T&gt;.
        /// </summary>
        /// <param name="val"></param>
        public static implicit operator Optional<T>(T val) => new Optional<T>(val);
    }
}
