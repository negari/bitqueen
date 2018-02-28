CREATE TABLE `request` (
  `id` int(11) NOT NULL,
  `Currency` varchar(45) NOT NULL,
  `Price` int(11) NOT NULL,
  `Volume` double NOT NULL,
  `Type` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
