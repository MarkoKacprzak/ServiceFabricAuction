using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Richter.Utilities;

namespace SFAuction.Common {
   /// <summary>The email is case preserved but case insensitive.</summary>
   [DataContract]
   public struct Email : IEquatable<Email>, IComparable<Email> {
      #region Static members
      private const int c_MaxCharacters = 100;
      public static readonly Email Empty = new Email(string.Empty);
      private static readonly Crc32 s_crc32 = new Crc32();
      private static readonly IdnMapping s_idnMapping = new IdnMapping();
      private const string c_emailRegex = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
         @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";

      private static bool IsValidEmailFormat(string email) {
         // Based on http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
         try {
            // Use IdnMapping class to convert Unicode domain names. 
            email = Regex.Replace(email, @"(@)(.+)$", match => {
               var domainName = match.Groups[2].Value;
               domainName = s_idnMapping.GetAscii(domainName);
               return match.Groups[1].Value + domainName;
            }, RegexOptions.None);
            return Regex.IsMatch(email, c_emailRegex, RegexOptions.IgnoreCase);
         }
         // Thrown by DomainMapper's IdnMapping.GetAscii call
         catch (ArgumentException) { return false; }
      }
      public static Email Parse(string email, bool trustInput = false) {
         if (!trustInput) {
            if (email != null) email = email.Trim();
            if (email.IsNullOrWhiteSpace() || email.Length > c_MaxCharacters || !IsValidEmailFormat(email))
               throw new InvalidEmailFormatException(email);
         }
         return new Email(email);
      }
      public static bool operator ==(Email email1, Email email2) => email1.Equals(email2);
      public static bool operator !=(Email email1, Email email2) => !email1.Equals(email2);
      #endregion

      [DataMember]
      private readonly string m_email;
      private Email(string email) {
         if (email == null) throw new ArgumentNullException("email");
         m_email = email;
      }
      public bool IsEmpty => m_email.IsNullOrWhiteSpace();
      public override string ToString() => m_email;
      public string Key => m_email.ToLowerInvariant();
      public override bool Equals(object obj) => (obj is Email) && Equals((Email)obj);
      public bool Equals(Email other) => string.Equals(m_email, other.m_email, StringComparison.OrdinalIgnoreCase);
      public override int GetHashCode() => Key.GetHashCode();

      public int CompareTo(Email other) => string.Compare(m_email, other.m_email, StringComparison.OrdinalIgnoreCase);

      public int PartitionKey() => unchecked((int)s_crc32.Finish(Encoding.UTF8.GetBytes(Key)));
   }

   public sealed class InvalidEmailFormatException : Exception {
      public InvalidEmailFormatException(string attemptedEmail) { AttemptedEmail = attemptedEmail; }
      public string AttemptedEmail { get; private set; }
   }
}

