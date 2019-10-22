-- Table Versions
CREATE TABLE __Version (
	Id int NOT NULL,
	TimeUTC datetime NOT NULL,

	CONSTRAINT [PK_Version] PRIMARY KEY (Id)
)

-- Index on column TimeUTC
CREATE INDEX IDX_Version_TimeUTC
ON __Version (TimeUTC)
