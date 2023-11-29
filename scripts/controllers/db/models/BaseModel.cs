using MongoDB.Bson.Serialization.Attributes;

public class BaseModel
{
    [BsonElement("id")]
    public string Id;
}