using System;
using MongoDB.Bson;
using MongoDB.Driver;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    public class UserRepositoryTest : BaseIntegrationTest
    {
        private readonly UserRepository _userRepository;
        public UserRepositoryTest() : base("users")
        {
            _userRepository = new UserRepository(_mongoDBSettings);
        }

        private async Task<User> CreateTestUser()
        {
            var mongoClient = new MongoClient(_mongoDBSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(_mongoDBSettings.Value.DatabaseName);
            var collection = database.GetCollection<User>("Users");
            
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