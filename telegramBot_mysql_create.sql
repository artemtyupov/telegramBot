drop database mydb2;
create database mydb2;
use mydb2;
CREATE TABLE `Folders` (
	`idStorage` INT NOT NULL,
	`Name` VARCHAR(30) NOT NULL,
	`id` INT NOT NULL auto_increment,
	PRIMARY KEY (`id`)
);

CREATE TABLE `Files` (
	`id` INT NOT NULL auto_increment,
	`idFolder` INT NOT NULL ,
	`Name` VARCHAR(30) NOT NULL ,
	`idMessage` INT NOT NULL ,
	PRIMARY KEY (`id`)
);

CREATE TABLE `User` (
	`id` INT NOT NULL auto_increment,
	`Name` VARCHAR(30) NOT NULL ,
	PRIMARY KEY (`id`)
);


CREATE TABLE `Storage` (
	`id` INT NOT NULL auto_increment,
	`idUser` INT NOT NULL ,
	`idShared` VARCHAR(36) NOT NULL ,
	PRIMARY KEY (`id`)
);


ALTER TABLE `Folders` ADD CONSTRAINT `Folders_fk0` FOREIGN KEY (`idStorage`) REFERENCES `Storage`(`id`);

ALTER TABLE `Files` ADD CONSTRAINT `Files_fk0` FOREIGN KEY (`idFolder`) REFERENCES `Folders`(`id`);

ALTER TABLE `Storage` ADD CONSTRAINT `Storage_fk0` FOREIGN KEY (`idUser`) REFERENCES `User`(`id`);
