Create Table Nudges
(
id int IDENTITY(0,1) PRIMARY KEY,
title varchar(255),
invitation varchar(255),
base64cover text,
base64icon text,
schedule datetime
)

Create Table EventNudges (
nid int not null,
eid int not null,

FOREIGN KEY (eid) REFERENCES Events(id),
FOREIGN KEY (nid) REFERENCES Nudges(id)
)