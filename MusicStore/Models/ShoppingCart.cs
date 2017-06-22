using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MusicStore.Models
{
    public class ShoppingCart
    {
        MusicStoreEntities storeDB = new MusicStoreEntities();

        string ShoppingCartID { get; set; }

        public const string CartSessionKey = "CartId";

        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartID = cart.GetCartId(context);
            return cart;
        }

        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        public void AddToCart(Album album)
        {
            //get matching cart and album instances
            var cartItem = storeDB.Carts.SingleOrDefault(
                c => c.CartID == ShoppingCartID
                && c.AlbumId == album.AlbumId);

            if(cartItem == null)
            {
                //create new cart item if no cart item exists
                cartItem = new Cart
                {
                    AlbumId = album.AlbumId,
                    CartID = ShoppingCartID,
                    Count = 1,
                    DateCreated = DateTime.Now
                };
                storeDB.Carts.Add(cartItem);
            } // end if

            else
            {
                //if cartItem does exist in the cart then add one to the qty
                cartItem.Count++;
            } // end else
            storeDB.SaveChanges();
        } // end method AddToCart

        public int RemoveFromCart(int id)
        {
            //get the cart
            var cartItem = storeDB.Carts.Single(
                c => c.CartID == ShoppingCartID
                && c.RecordId == id);
            int itemCount = 0;

            if(cartItem !=null)
            {
                if(cartItem.Count >1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                } // end if
                else
                {
                    storeDB.Carts.Remove(cartItem);
                } // end else
                storeDB.SaveChanges();
            } // end if
            return itemCount;
        } //end method RemoveFromCart

        public void EmptyCart()
        {
            var cartItems = storeDB.Carts.Where(c => c.CartID == ShoppingCartID);
            foreach(var item in cartItems)
            {
                storeDB.Carts.Remove(item);
            }
            //save changes
            storeDB.SaveChanges();
        } // end method EmptyCart

        public List<Cart> GetCartItems()
        {
            return storeDB.Carts.Where(c => c.CartID == ShoppingCartID).ToList();
        } // end method GetCartItems

        public int GetCount()
        {
            //get the count of each item in the cart and sum them up
            int? count = (from cartItems in storeDB.Carts
                          where cartItems.CartID == ShoppingCartID
                          select (int?)cartItems.Count).Sum();
            //return 0 if all entries are null
            return count ?? 0;
        } // end method GetCount

        public decimal GetTotal()
        {
            // Multiply album price by count of that album to get 
            // the current price for each of those albums in the cart
            // sum all album price totals to get the cart total
            decimal? total = (from cartItems in storeDB.Carts
                              where cartItems.CartID == ShoppingCartID
                              select (int?)cartItems.Count * cartItems.Album.Price).Sum();

                return total ?? decimal.Zero;
        } // end method GetTotal

        public int CreateOrder(Order order)
        {
            decimal orderTotal = 0;
            var cartItems = GetCartItems();

            // iterate over the items in the cart
            foreach(var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    AlbumId = item.AlbumId,
                    OrderId = order.OrderId,
                    UnitPrice = item.Album.Price,
                    Quantity = item.Count
                };
                //set the order total to the orderTotal count
                orderTotal += (item.Count * item.Album.Price);

                storeDB.OrderDetails.Add(orderDetail);
            } // end foreach

            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Save the order
            storeDB.SaveChanges();

            // Empty the shopping cart
            EmptyCart();

            // Return the OrderId as the confirmation number
            return order.OrderId;
        } // end method CreateOrder

        // We're using HttpContextBase to allow access to cookies.
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] =
                        context.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class
                    Guid tempCartId = Guid.NewGuid();
                    // Send tempCartId back to client as a cookie
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return context.Session[CartSessionKey].ToString();
        } // end method GetCartId

        // When a user has logged in, migrate their shopping cart to
        // be associated with their username
        public void MigrateCart(string userName)
        {
            var shoppingCart = storeDB.Carts.Where(
                c => c.CartID == ShoppingCartID);

            foreach (Cart item in shoppingCart)
            {
                item.CartID = userName;
            }
            storeDB.SaveChanges();
        } // end method MigrateCart

    } // end class ShoppingCart
}