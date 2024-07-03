using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Order_Api.Helper;
using Order_Api.Model;
using Order_Api.Repository;
using Xunit;

namespace Order_Api.Tests
{
    public class OrderRepositoryTests : IDisposable
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly string _testCollectionName = "Order";

        public OrderRepositoryTests()
        {
            GoogleCredential credential;
            if (Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS") != null)
            {
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS"))))
                {
                    credential = GoogleCredential.FromStream(stream);
                }
            }
            else
            {
                using (var stream = new FileStream("firebase_credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream);
                }
            }

            var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECTID") ?? JsonReader.GetFieldFromJsonFile("project_id");
            var builder = new FirestoreDbBuilder
            {
                Credential = credential,
                ProjectId = projectId,
                DatabaseId = "test"  // Use the 'test' database for testing
            };
            _firestoreDb = builder.Build();
        }

        [Fact]
        public async Task CreateOrder_Should_Add_Order_To_Firestore()
        {
            var orderRepository = new OrderRepository(_firestoreDb);
            var order = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = "user123",
                Products = new List<string> { "product1", "product2" }
            };
            var orderId = await orderRepository.CreateOrder(order);
            Assert.NotNull(orderId);
            var retrievedOrder = await orderRepository.GetOrderById(orderId);
            AssertOrderProperties(order, retrievedOrder);
            await orderRepository.DeleteOrder(orderId);
        }

        [Fact]
        public async Task GetOrderById_Should_Retrieve_Order_From_Firestore()
        {
            var orderRepository = new OrderRepository(_firestoreDb);
            var order = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = "user123",
                Products = new List<string> { "product1", "product2" }
            };
            var orderId = await orderRepository.CreateOrder(order);
            var retrievedOrder = await orderRepository.GetOrderById(orderId);
            Assert.NotNull(retrievedOrder);
            AssertOrderProperties(order, retrievedOrder);
            await orderRepository.DeleteOrder(orderId);
        }

        [Fact]
        public async Task GetAllOrders_Should_Retrieve_All_Orders_From_Firestore()
        {
            var orderRepository = new OrderRepository(_firestoreDb);
            var order1 = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = "user123",
                Products = new List<string> { "product1", "product2" }
            };
            var order2 = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = "user456",
                Products = new List<string> { "product3", "product4" }
            };
            var orderId1 = await orderRepository.CreateOrder(order1);
            var orderId2 = await orderRepository.CreateOrder(order2);

            var orders = await orderRepository.GetAllOrders();
            Assert.NotNull(orders);
            Assert.True(orders.Count >= 2);

            await orderRepository.DeleteOrder(orderId1);
            await orderRepository.DeleteOrder(orderId2);
        }

        [Fact]
        public async Task UpdateOrder_Should_Update_Order_In_Firestore()
        {
            var orderRepository = new OrderRepository(_firestoreDb);
            var order = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = "user123",
                Products = new List<string> { "product1", "product2" }
            };
            var orderId = await orderRepository.CreateOrder(order);

            // Modify order details
            order.Products = new List<string> { "product3", "product4" };

            // Act
            await orderRepository.UpdateOrder(orderId, order);
            var updatedOrder = await orderRepository.GetOrderById(orderId);

            // Assert
            Assert.Equal(order.Products, updatedOrder.Products);

            // Clean up
            await orderRepository.DeleteOrder(orderId);
        }

        [Fact]
        public async Task DeleteOrder_Should_Delete_Order_From_Firestore()
        {
            var orderRepository = new OrderRepository(_firestoreDb);
            var order = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = "user123",
                Products = new List<string> { "product1", "product2" }
            };
            var orderId = await orderRepository.CreateOrder(order);

            // Act
            await orderRepository.DeleteOrder(orderId);
            var deletedOrder = await orderRepository.GetOrderById(orderId);

            // Assert
            Assert.Null(deletedOrder);
        }

        [Fact]
        public async Task GetOrdersByUserId_Should_Retrieve_Orders_By_UserId_From_Firestore()
        {
            var orderRepository = new OrderRepository(_firestoreDb);
            var userId = "user123";
            var order1 = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Products = new List<string> { "product1", "product2" }
            };
            var order2 = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Products = new List<string> { "product3", "product4" }
            };
            var orderId1 = await orderRepository.CreateOrder(order1);
            var orderId2 = await orderRepository.CreateOrder(order2);

            var orders = await orderRepository.GetOrdersByUserId(userId);
            Assert.NotNull(orders);
            Assert.Equal(2, orders.Count);

            await orderRepository.DeleteOrder(orderId1);
            await orderRepository.DeleteOrder(orderId2);
        }

        [Fact]
        public async Task DeleteOrdersByIds_Should_Delete_Orders_From_Firestore()
        {
            var orderRepository = new OrderRepository(_firestoreDb);
            var order1 = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = "user123",
                Products = new List<string> { "product1", "product2" }
            };
            var order2 = new Order
            {
                CreatedAt = DateTime.UtcNow,
                UserId = "user456",
                Products = new List<string> { "product3", "product4" }
            };
            var orderId1 = await orderRepository.CreateOrder(order1);
            var orderId2 = await orderRepository.CreateOrder(order2);

            await orderRepository.DeleteOrdersByIds(new List<string> { orderId1, orderId2 });

            var deletedOrder1 = await orderRepository.GetOrderById(orderId1);
            var deletedOrder2 = await orderRepository.GetOrderById(orderId2);

            Assert.Null(deletedOrder1);
            Assert.Null(deletedOrder2);
        }

        private void AssertOrderProperties(Order expected, Order actual)
        {
            Assert.Equal(expected.UserId, actual.UserId);
            Assert.Equal(expected.Products, actual.Products);
        }

        public void Dispose()
        {
            // Clean up Firestore data after tests
            ClearFirestoreCollection(_testCollectionName);
        }

        private async void ClearFirestoreCollection(string collectionName)
        {
            var collectionRef = _firestoreDb.Collection(collectionName);
            var query = collectionRef;
            var batch = _firestoreDb.StartBatch();

            // Batch delete documents
            var querySnapshot = await query.GetSnapshotAsync();
            foreach (var documentSnapshot in querySnapshot.Documents)
            {
                batch.Delete(documentSnapshot.Reference);
            }

            // Commit the batch
            await batch.CommitAsync();
        }
    }
}
