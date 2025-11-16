using System;
using MongoDB.Bson;
using MongoDB.Driver;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    [Trait("Category", "Repository")]
    public class UserRepositoryTest : BaseRepositoryTest
    {
        private UserRepository _userRepository;
        public UserRepositoryTest() : base("users")
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _userRepository = new UserRepository(Database, MongoDbSettings);
        }
        private async Task<User> CreateTestUser()
        {
            var collection = Database.GetCollection<User>(MongoDbSettings.Value.UsersCollectionName);
            
            var user = new User
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FullName = "Test User",
                Email = "test@email.com",
                NotificationType = Type.Email
            };
            
            await collection.InsertOneAsync(user);
            return user;
        }

        [Fact]
        public async Task GetUserByIdOK()
        {
            var user = await CreateTestUser();

            var fetchedUser = await _userRepository.GetByIdAsync(user.Id);

            Assert.NotNull(fetchedUser);
            Assert.Equal(user.Id, fetchedUser.Id);
            Assert.Equal(user.FullName, fetchedUser.FullName);
            Assert.Equal(user.Email, fetchedUser.Email);
            Assert.Equal(user.NotificationType, fetchedUser.NotificationType);
        }

        [Fact]
        public async Task GetUserByIdNotOK_ThrowsException()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                async () => await _userRepository.GetByIdAsync(nonExistentId)
            );

            Assert.Equal("User", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task GetUserByIdNotOK_IdIsNull_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                async () => await _userRepository.GetByIdAsync(null)
            );

            Assert.Equal("User", exception.DocumentType);
            Assert.Null(exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }
    }
}