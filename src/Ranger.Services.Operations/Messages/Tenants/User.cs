namespace Ranger.Services.Operations {
    public class User {
        public User (string email, string firstName, string lastName, string passwordHash) {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.PasswordHash = passwordHash;

        }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string PasswordHash { get; }
    }
}