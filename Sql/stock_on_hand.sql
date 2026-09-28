USE ProcurementDb;

SELECT Sku,
       Name,
       StockQuantity,
       StockQuantity * UnitPrice AS StockValue
FROM Products
ORDER BY Name;