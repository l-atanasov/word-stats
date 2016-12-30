CREATE TABLE exams (
	id INTEGER PRIMARY KEY ASC AUTOINCREMENT,
	name VARCHAR(255) NOT NULL,
	content TEXT NOT NULL
);

CREATE TABLE words (
	id INTEGER PRIMARY KEY ASC AUTOINCREMENT,
	value TEXT NOT NULL
);

CREATE TABLE exam_words_count (
	exam_id INTEGER NOT NULL,
	word_id INTEGER NOT NULL,
	word_count INTEGER NOT NULL,
	PRIMARY KEY(exam_id, word_id),
	FOREIGN KEY(exam_id) REFERENCES exams(id),
	FOREIGN KEY(word_id) REFERENCES words(id)
);

CREATE TABLE metadata (
	key VARCHAR(32) PRIMARY KEY,
	value VARCHAR(255) NOT NULL
);

INSERT INTO metadata (key, value) VALUES ('version', '1.0');