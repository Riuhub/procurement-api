USE ProcurementDb;

SELECT v.Name AS Vendor,
       COUNT(DISTINCT o.Id) AS OpenOrders,
       ISNULL(SUM(l.Quantity * l.UnitPrice), 0) AS OpenValue
FROM Vendors v
LEFT JOIN PurchaseOrders o
       ON o.VendorId = v.Id AND o.Status = 'Draft'
LEFT JOIN PurchaseOrderLines l
       ON l.PurchaseOrderId = o.Id
GROUP BY v.Id , v.Name
ORDER BY OpenValue DESC;