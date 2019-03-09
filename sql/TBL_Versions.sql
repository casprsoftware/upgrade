-- Table Versions
CREATE TABLE __Versions (
	Id int NOT NULL,
	--Description nvarchar(200) NOT NULL,
	TimeUTC datetime NOT NULL,

	CONSTRAINT [PK_Versions] PRIMARY KEY (Id)
)

-- Index on column TimeUTC
CREATE INDEX IDX_Versions_TimeUTC
ON __Versions (TimeUTC)
