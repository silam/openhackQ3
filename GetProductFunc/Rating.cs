using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



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
