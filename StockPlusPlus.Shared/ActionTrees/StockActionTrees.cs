
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace StockPlusPlus.Shared.ActionTrees;

[ActionTree("Stock", "Stock")]
public class StockActionTrees
{
    public readonly static ReadWriteDeleteAction Brand = new ReadWriteDeleteAction("Brand");
    public readonly static ReadWriteDeleteAction ProductCategory = new ReadWriteDeleteAction("Product Category");
    public readonly static ReadWriteDeleteAction Product = new ReadWriteDeleteAction("Product");

    [ActionTree("Data Level Access", "Data Level or Row-Level Access")]
    public class DataLevelAccess
    {
        public readonly static DynamicReadWriteDeleteAction Brand = new DynamicReadWriteDeleteAction("Brand"); 
        public readonly static DynamicReadWriteDeleteAction ProductCategory = new DynamicReadWriteDeleteAction("Product Category"); 
    }
}
