-----------------------
-- Write model queries
-----------------------
USE [Bookstore-WriteModel];
SELECT * FROM [Orders] ORDER BY [OrderNumber];
SELECT * FROM [Events] ORDER BY [OrderNumber], [Version];

-----------------------
-- Read model queries
-----------------------
USE [Bookstore-ReadModel];
SELECT * FROM [Orders] ORDER BY [OrderNumber];
SELECT [OrderNumber], [ProductNumber], [Price] FROM [Products] ORDER BY [OrderNumber];

-----------------------
-- Shipping queries
-----------------------
USE [Bookstore-Shipping];
SELECT * FROM [Orders] ORDER BY [OrderNumber];
SELECT [OrderNumber], [ProductNumber] FROM [Products] ORDER BY [OrderNumber];
SELECT * FROM [ShippingInfo];
