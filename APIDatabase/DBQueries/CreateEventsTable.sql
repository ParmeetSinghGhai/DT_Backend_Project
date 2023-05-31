create table Events
(
id int not null IDENTITY(0,1) PRIMARY KEY,
name varchar(255) not null,
tagline varchar(255) not null,
schedule datetime not null,
description varchar(max),
rigor_rank int not null,
category varchar(255) not null,
sub_category varchar(255) not null,
)
