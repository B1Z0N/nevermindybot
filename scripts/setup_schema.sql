
CREATE TABLE IF NOT EXISTS MessageToItsDeletionJobId (
   ChatId INT8 NOT NULL, 
   MessageId INT4 NOT NULL,
   DeletionJobId TEXT,
   PRIMARY KEY (ChatId, MessageId)
);

ALTER TABLE MessageToItsDeletionJobId OWNER TO nevermindy; 
