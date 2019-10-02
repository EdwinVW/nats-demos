use [Bookstore-WriteModel];
delete from Events;
delete from Orders;

use [Bookstore-ReadModel];
delete from products;
delete from orders;

use [Bookstore-Shipping];
delete from products;
delete from orders;
delete from shippinginfo;
