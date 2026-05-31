using System;
using System.Collections.Generic;

namespace ECommerce.CartService.Domain.Events;

public record CartCheckedOutEvent(
    Guid CartId, 
    string UserId, 
    List<CartCheckedOutItem> Items, 
    DateTime CheckedOutAt);

public record CartCheckedOutItem(Guid ProductId, int Quantity);
