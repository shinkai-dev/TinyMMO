using MongoDB.Driver;
using MongoDB.Bson;
using Godot;

public abstract class DbController<T> where T : BaseModel
{
    private static readonly MongoClient Client = new MongoClient(DbConsts.MONGO_URL);
    private static readonly IMongoDatabase Database = Client.GetDatabase(DbConsts.DATABASE_NAME);
    private IMongoCollection<T> Collection;

    public DbController(string DefaultCollection)
    {
        ChangeCollection(DefaultCollection);
    }

    protected void ChangeCollection(string name)
    {
        Collection = Database.GetCollection<T>(name);
    }

    public void SetDoc(T doc)
    {
        Collection.InsertOne(doc);
    }

    public T GetDoc(string id)
    {
        var filter = Builders<T>.Filter.Eq(r => r.Id, id);
        return Collection.Find(filter).FirstOrDefault();
    }

    public T[] QueryBy(string field, string value)
    {
        var filter = Builders<T>.Filter.Eq(field, value);
        return Collection.Find(filter).ToList().ToArray();
    }
}