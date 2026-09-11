using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects;
using System.Runtime.Serialization;

namespace Reinsurance.Core
{
    [DataContract]
    public abstract class BaseEntity : IEquatable<BaseEntity>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public int Id { get; set; }

        public virtual string GetEntityName()
        {
            return GetUnproxiedType().Name;
        }

        public Type GetUnproxiedType()
        {
            return ObjectContext.GetObjectType(GetType());
        }

        public virtual bool IsTransientRecord()
        {
            return Id == 0;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }

        protected virtual bool Equals(BaseEntity other)
        {
            return other != null && (ReferenceEquals(this, other) ||
                (!IsTransientRecord() && !other.IsTransientRecord() &&
                 Id == other.Id && GetUnproxiedType() == other.GetUnproxiedType()));
        }

        bool IEquatable<BaseEntity>.Equals(BaseEntity other)
        {
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return IsTransientRecord() ? base.GetHashCode() : (GetUnproxiedType().GetHashCode() * 31) ^ Id;
        }

        public static bool operator ==(BaseEntity left, BaseEntity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseEntity left, BaseEntity right)
        {
            return !Equals(left, right);
        }
    }
}
