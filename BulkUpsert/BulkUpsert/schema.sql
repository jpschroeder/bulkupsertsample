create database mydb
go

USE [mydb]
GO

-- select * from sometable
CREATE TABLE [dbo].[sometable](
	[sometable_id] [bigint] IDENTITY(1,1) NOT NULL,
	[unique_field] [int] null,
	[field1] [int] null,
	[field2] [varchar](max) null,
	[field3] [bit] null,

 CONSTRAINT [pk_sometable] PRIMARY KEY CLUSTERED 
(
	[sometable_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TYPE [dbo].[sometable_type] AS TABLE(
	[unique_field] [int] null,
	[field1] [int] null,
	[field2] [varchar](max) null,
	[field3] [bit] null
)
GO

create PROCEDURE [dbo].[sometable_upsert] (
	@data [dbo].[sometable_type] READONLY
)
AS
DECLARE @T TABLE([id] int, [_rownumber] int)

MERGE INTO [dbo].[sometable] AS t
USING (SELECT *, [_rownumber] = ROW_NUMBER() OVER (ORDER BY (SELECT 1)) FROM @data) AS s
ON
(
	t.[unique_field] = s.[unique_field]
)
WHEN MATCHED THEN UPDATE SET
	t.[field1] = s.[field1],
	t.[field2] = s.[field2],
	t.[field3] = s.[field3]

WHEN NOT MATCHED BY TARGET THEN INSERT
(
	[unique_field],
	[field1],
	[field2],
	[field3]
)
VALUES
(
	s.[unique_field],
	s.[field1],
	s.[field2],
	s.[field3]
)
OUTPUT Inserted.[sometable_id], s.[_rownumber] INTO @T ;

SELECT [id] FROM @T ORDER BY [_rownumber]

go

--grant execute on sometable_upsert to myuser
--grant execute on TYPE::[dbo].[sometable_type] to myuser