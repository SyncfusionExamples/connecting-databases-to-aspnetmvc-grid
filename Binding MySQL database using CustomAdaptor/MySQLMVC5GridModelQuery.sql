-- Create database + select it
CREATE DATABASE IF NOT EXISTS transactiondb
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_general_ci;
USE transactiondb;

-- Create table
CREATE TABLE IF NOT EXISTS transactions (
  Id               INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  TransactionId    VARCHAR(50) NOT NULL UNIQUE,
  CustomerId       INT NOT NULL,
  OrderId          INT NULL,
  InvoiceNumber    VARCHAR(50) NULL,
  Description      VARCHAR(500) NULL,
  Amount           DECIMAL(15,2) NOT NULL,
  CurrencyCode     VARCHAR(10) NULL,
  TransactionType  VARCHAR(50) NULL,
  PaymentGateway   VARCHAR(100) NULL,
  CreatedAt        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CompletedAt      DATETIME NULL,
  Status           VARCHAR(50) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Seed data
INSERT INTO transactions
  (TransactionId, CustomerId, OrderId, InvoiceNumber, Description, Amount, CurrencyCode,
   TransactionType, PaymentGateway, CreatedAt, CompletedAt, Status)
VALUES
('TXN260113001', 1001, 50001, 'INV-2026-001', 'Samsung S25 Ultra', 153399.00, 'INR',
 'SALE', 'Razorpay', '2026-01-13 10:15:30', '2026-01-13 10:16:55', 'SUCCESS'),
('TXN260113002', 1002, 50002, 'INV-2026-002', 'MacBook Pro M4', 224199.00, 'INR',
 'SALE', 'Stripe', '2026-01-13 11:20:10', '2026-01-13 11:21:40', 'SUCCESS');