INSERT INTO Storage (Name, idUser, idShared) VALUES ("test_storage1", 1, "test_share_key1");
INSERT INTO Storage (Name, idUser, idShared) VALUES ("test_storage2", 1, "test_share_key2");
INSERT INTO Folders (idStorage, idFolder, Name) VALUES (1, -1, "test_folder1");
INSERT INTO Folders (idStorage, idFolder, Name) VALUES (1, -1, "test_folder2");
INSERT INTO Folders (idStorage, idFolder, Name) VALUES (2, -1, "test_folder3");
INSERT INTO Folders (idStorage, idFolder, Name) VALUES (2, -1, "test_folder4");