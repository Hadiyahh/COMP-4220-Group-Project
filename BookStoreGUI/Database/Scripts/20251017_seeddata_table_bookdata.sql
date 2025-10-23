INSERT INTO dbo.BookData
(ISBN, CategoryID, Title, Author, Price, SupplierId, [Year], Edition, Publisher, InStock)
VALUES
('0141182636', 1, '1984', 'George Orwell', 12.99, 1, '2021', '1', N'Penguin', 30),
('0061120081', 1, 'To Kill a Mockingbird', 'Harper Lee', 14.50, 1, '2020', '2', N'Harper', 25),
('0743273567', 1, 'The Great Gatsby', 'F. Scott Fitzgerald', 11.99, 1, '2019', '3', N'Scribner', 40),
('0451524934', 1, 'Animal Farm', 'George Orwell', 10.50, 1, '2018', '2', N'Signet', 35),
('0141439602', 1, 'Pride and Prejudice', 'Jane Austen', 13.25, 1, '2022', '1', N'Penguin', 28),

('0132350882', 2, 'Clean Code', 'Robert C. Martin', 42.99, 2, '2019', '1', N'Prentice', 15),
('0262033844', 2, 'Intro to Algorithms', 'Thomas H. Cormen', 85.00, 2, '2022', '4', N'MITPress', 10),
('0596007124', 2, 'Head First Design Patterns', 'Eric Freeman', 48.99, 3, '2020', '2', N'OReilly', 12),
('0131103628', 2, 'The C Programming Language', 'Brian W. Kernighan', 65.00, 3, '2015', '2', N'Prentice', 18),
('1491950358', 2, 'Learning Python', 'Mark Lutz', 59.99, 3, '2021', '5', N'OReilly', 20),

('0596805527', 3, 'JavaScript: The Good Parts', 'Douglas Crockford', 29.99, 3, '2018', '1', N'OReilly', 22),
('161729329X', 3, 'Spring in Action', 'Craig Walls', 49.99, 4, '2019', '5', N'Manning', 16),
('0134685997', 3, 'Effective Java', 'Joshua Bloch', 54.50, 4, '2018', '3', N'Addison', 14),
('149207800X', 3, 'Fluent Python', 'Luciano Ramalho', 64.75, 4, '2022', '2', N'OReilly', 10),
('1492056817', 3, 'Data Science from Scratch', 'Joel Grus', 55.00, 4, '2019', '2', N'OReilly', 9),

('0062316117', 4, 'The Alchemist', 'Paulo Coelho', 18.75, 5, '2018', '1', N'Harper', 30),
('0143127748', 4, 'The Power of Habit', 'Charles Duhigg', 19.99, 5, '2016', '1', N'Random', 25),
('1982137274', 4, 'Atomic Habits', 'James Clear', 21.50, 5, '2021', '1', N'Avery', 35),
('0062457713', 4, 'Think Like a Monk', 'Jay Shetty', 23.99, 5, '2020', '1', N'Simon', 20),
('0307465357', 4, 'Start With Why', 'Simon Sinek', 22.49, 5, '2011', '1', N'Portfolio', 18),

('1451648537', 5, 'Steve Jobs', 'Walter Isaacson', 28.99, 1, '2019', '1', N'Simon', 15),
('038552883X', 5, 'Elon Musk', 'Ashlee Vance', 27.99, 1, '2017', '1', N'Harper', 14),
('0062312681', 5, 'Becoming', 'Michelle Obama', 26.50, 1, '2020', '1', N'Crown', 18),
('1501139151', 5, 'Shoe Dog', 'Phil Knight', 24.75, 1, '2018', '1', N'Scribner', 22),
('1591846447', 5, 'The Lean Startup', 'Eric Ries', 25.00, 1, '2014', '1', N'Crown', 17),

('0143110434', 6, 'The Intelligent Investor', 'Benjamin Graham', 34.99, 2, '2015', '1', N'Harper', 12),
('1476757801', 6, 'Rich Dad Poor Dad', 'Robert Kiyosaki', 17.99, 2, '2018', '2', N'Plata', 40),
('0066620996', 6, 'Good to Great', 'Jim Collins', 29.99, 2, '2016', '1', N'Harper', 25),
('0804139296', 6, '7 Habits of Highly Effective', 'Stephen R. Covey', 19.99, 2, '2020', '1', N'FreePress', 28),
('125021778X', 6, 'Principles: Life and Work', 'Ray Dalio', 39.99, 2, '2019', '1', N'Simon', 12);
