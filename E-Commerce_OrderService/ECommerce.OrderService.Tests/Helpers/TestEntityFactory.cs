using System;
using System.Collections.Generic;
using System.Reflection;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Domain.Enums;

namespace ECommerce.OrderService.Tests.Helpers;

public static class TestEntityFactory
{
    public static Order CreateOrderWithPrivateFields(
        Guid id,
        string userId,
        decimal totalPrice,
        OrderStatus status,
        string shippingAddress,
        PaymentStatus paymentStatus,
        DateTime createdAt,
        List<OrderItem> items,
        string? paymobTransactionId = null,
        string? refundTransactionId = null)
    {
        var constructor = typeof(Order).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            Type.EmptyTypes,
            null);

        if (constructor == null)
            throw new InvalidOperationException("Private parameterless constructor for Order not found.");

        var order = (Order)constructor.Invoke(null);

        SetPrivateField(order, "<Id>k__BackingField", id);
        SetPrivateField(order, "<UserId>k__BackingField", userId);
        SetPrivateField(order, "<TotalPrice>k__BackingField", totalPrice);
        SetPrivateField(order, "<Status>k__BackingField", status);
        SetPrivateField(order, "<ShippingAddress>k__BackingField", shippingAddress);
        SetPrivateField(order, "<PaymentStatus>k__BackingField", paymentStatus);
        SetPrivateField(order, "<CreatedAt>k__BackingField", createdAt);
        SetPrivateField(order, "<PaymobTransactionId>k__BackingField", paymobTransactionId);
        SetPrivateField(order, "<RefundTransactionId>k__BackingField", refundTransactionId);

        // Add items to _items private field
        var itemsField = typeof(Order).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        if (itemsField != null)
        {
            var list = (List<OrderItem>)itemsField.GetValue(order)!;
            list.AddRange(items);
        }

        return order;
    }

    private static void SetPrivateField(object obj, string fieldName, object? value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            // Try matching by property name if backing field is different
            var propName = fieldName.Replace("<", "").Replace(">k__BackingField", "");
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value);
            }
            else
            {
                // Try finding field with case insensitivity or underscore
                var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var f in fields)
                {
                    if (f.Name.Equals(propName, StringComparison.OrdinalIgnoreCase) || 
                        f.Name.Equals($"_{propName}", StringComparison.OrdinalIgnoreCase))
                    {
                        f.SetValue(obj, value);
                        return;
                    }
                }
            }
        }
    }
}
