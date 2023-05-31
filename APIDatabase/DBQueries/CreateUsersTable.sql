create table Users
(
id int IDENTITY(0,1) PRIMARY KEY,
eid int not null,
type varchar(255),
firstname varchar(255) not null,
lastname varchar(255) not null,


FOREIGN KEY (eid) REFERENCES Events(id),
CONSTRAINT CHECK_TYPE CHECK (type IN ('owner', 'moderator', 'attendee'))
)