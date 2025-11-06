using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Requests
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        [RegularExpression(@"^[a-fA-F0-9]{24}$", ErrorMessage = "UserId must be a valid ObjectId")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Item property is required")]
        public OrderItemRequest Item { get; set; }
    }

    public class OrderItemRequest
    {
        [Required(ErrorMessage = "ProductId is required")]
        [RegularExpression(@"^[a-fA-F0-9]{24}$", ErrorMessage = "ProductId must be a valid ObjectId")]
        public string ProductId { get; set; }
        [Range(1,5, ErrorMessage = "Quantity must be between 1 and 5")]
        public int Quantity { get; set; }
        //TODO: Move from handler and fetch from db in handler
        public decimal UnitPrice { get; set; }
    }
}