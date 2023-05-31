Create Table Files
(
id int IDENTITY(0,1) PRIMARY KEY,
eid int not null,
name varchar(255),
base64data text,

FOREIGN KEY (eid) REFERENCES Events(id)
)
