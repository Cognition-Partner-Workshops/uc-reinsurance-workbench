using System.Collections.Generic;
using Reinsurance.Services.Cedents.Models;

namespace Reinsurance.Services.Cedents
{
    public partial interface ICedentService
    {
        CedentModel GetById(int cedentId);
        IList<CedentModel> GetAll(bool activeOnly = true);
        void Insert(CedentModel model);
        void Update(CedentModel model);
    }
}
