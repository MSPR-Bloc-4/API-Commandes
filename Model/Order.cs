using Google.Cloud.Firestore;

namespace Order_Api.Model;

[FirestoreData]
public class Order
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public DateTime CreatedAt { get; set; }

    [FirestoreProperty]
    public string UserId { get; set; }

    [FirestoreProperty]
    public List<string> Products { get; set; }
}