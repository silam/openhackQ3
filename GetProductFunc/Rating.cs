using System;

namespace GetProductFunc
{
    public record Rating(
        Guid id,
        Guid productid,
        Guid userId,
        string locationName,
        int rating,
        string userNotes,
        DateTime timestamp
    );
}