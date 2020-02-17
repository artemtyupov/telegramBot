drop database TelegramBot;
create database TelegramBot;
use TelegramBot;
CREATE TABLE `Folders` (
	`idStorage` INT NOT NULL,
	`Name` VARCHAR(15) NOT NULL,
	`id` INT NOT NULL auto_increment,
	PRIMARY KEY (`id`)
);

CREATE TABLE `Files` (
	`id` INT NOT NULL auto_increment,
	`idFolder` INT NOT NULL ,
	`Name` VARCHAR(15) NOT NULL ,
	`idMessage` INT NOT NULL ,
	`idChat` INT NOT NULL ,
	PRIMARY KEY (`id`)
);

CREATE TABLE `User` (
	`id` INT NOT NULL auto_increment,
	`Name` VARCHAR(300) NOT NULL ,
	PRIMARY KEY (`id`)
);


CREATE TABLE `Storage` (
	`id` INT NOT NULL auto_increment,
	`Name` VARCHAR(15) NOT NULL ,
	`idUser` INT NOT NULL ,
	`idShared` VARCHAR(300) NOT NULL ,
	PRIMARY KEY (`id`)
);


ALTER TABLE `Folders` ADD CONSTRAINT `Folders_fk0` FOREIGN KEY (`idStorage`) REFERENCES `Storage`(`id`);

ALTER TABLE `Files` ADD CONSTRAINT `Files_fk0` FOREIGN KEY (`idFolder`) REFERENCES `Folders`(`id`);

ALTER TABLE `Storage` ADD CONSTRAINT `Storage_fk0` FOREIGN KEY (`idUser`) REFERENCES `User`(`id`);

DELIMITER //

CREATE TRIGGER foldersBeforeDelete
BEFORE DELETE
   ON Folders FOR EACH ROW

BEGIN
	DELETE FROM Files WHERE idFolder = OLD.id;
END; //

DELIMITER ;

DELIMITER //

CREATE TRIGGER storageBeforeDelete
BEFORE DELETE
   ON Storage FOR EACH ROW

BEGIN
	DELETE FROM Folders WHERE idStorage = OLD.id;
END; //

DELIMITER ;