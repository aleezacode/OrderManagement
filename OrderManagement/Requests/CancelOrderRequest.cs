using System;
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Requests
{
    public class CancelOrderRequest
    {
        [Required(ErrorMessage = "OrderId is required")]
        [RegularExpression(@"^[a-fA-F0-9]{24}$", ErrorMessage = "OrderId must be a valid ObjectId")]
        public string OrderId { get; set; }
        [Required(ErrorMessage = "Reason is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Reason must be between 1 and 50 characters")]
        public string Reason { get; set; }
    }
}