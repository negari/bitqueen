CREATE TABLE `trades` (
  `Buyer_id` int(11) NOT NULL,
  `Seller_id` int(11) NOT NULL,
  `Currency` varchar(45) DEFAULT NULL,
  `Date` varchar(45) DEFAULT NULL,
  `Volume` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`Buyer_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
