using Google.Cloud.Firestore;
using Order_Api.Model;
using Order_Api.Repository.Interface;

namespace Order_Api.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly CollectionReference _collectionReference;

        public OrderRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
            _collectionReference = _firestoreDb.Collection("Order");
        }

        public async Task<string> CreateOrder(Order order)
        {
            var document = await _collectionReference.AddAsync(order);
            return document.Id;
        }

        public async Task<Order> GetOrderById(string orderId)
        {
            var document = await _collectionReference.Document(orderId).GetSnapshotAsync();
            return document.Exists ? document.ConvertTo<Order>() : null;
        }

        public async Task<List<Order>> GetAllOrders()
        {
            var querySnapshot = await _collectionReference.GetSnapshotAsync();
            List<Order> orders = new List<Order>();

            foreach (var document in querySnapshot.Documents)
            {
                orders.Add(document.ConvertTo<Order>());
            }

            return orders;
        }

        public async Task UpdateOrder(string orderId, Order order)
        {
            await _collectionReference.Document(orderId).SetAsync(order, SetOptions.Overwrite);
        }

        public async Task DeleteOrder(string orderId)
        {
            await _collectionReference.Document(orderId).DeleteAsync();
        }

        public async Task DeleteOrdersByUserId(string userId)
        {
            var querySnapshot = await _collectionReference
                .WhereEqualTo("UserId", userId)
                .GetSnapshotAsync();

            foreach (var document in querySnapshot.Documents)
            {
                await document.Reference.DeleteAsync();
            }
        }
    }
}
