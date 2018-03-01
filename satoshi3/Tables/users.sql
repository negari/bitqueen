CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `Username` varchar(45) DEFAULT NULL,
  `Name` varchar(100) DEFAULT NULL,
  `Family` varchar(200) DEFAULT NULL,
  `Address` varchar(500) DEFAULT NULL,
  `Phone` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
