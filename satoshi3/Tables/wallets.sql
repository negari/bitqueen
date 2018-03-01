CREATE TABLE `wallets` (
  `id` int(11) NOT NULL,
  `currency` varchar(45) NOT NULL,
  `private_key` varchar(500) DEFAULT NULL,
  `public_key` varchar(500) DEFAULT NULL,
  `user_id` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
