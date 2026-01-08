# Milestone 1 Project Report

**Module:** CS6004NP Application Development  
**Student Name:** [Your Name]  
**Student ID:** [Your ID]  

---

## a) Project Overview

The "Daily Journal" application is a comprehensive digital solution designed to modernize the traditional habit of personal journaling. In an era where mental health awareness and self-reflection are becoming increasingly vital, this application serves as a secure, private, and accessible platform for users to document their daily lives, thoughts, and emotions.

The core objective of this project is to provide a desktop-based software solution that mimics the intimacy of a physical diary while leveraging the conveniences of modern technology. Unlike standard text editors, the Daily Journal is purpose-built for reflection. It enforces a disciplined approach to writing by allowing only one journal entry per day, encouraging users to synthesize their thoughts into a meaningful daily record rather than fragmented notes.

Key features of the application include a robust **Rich Text / Markdown** editor. This allows writers to express themselves not just through words, but through structured formatting—emphasizing key points with bold text, organizing thoughts with lists, and ensuring their entries are readable and visually appealing. This addresses the requirement for a "feature-rich" writing environment that supports the creative needs of a writer.

Beyond simple text, the application integrates a **Mood Tracking System**. Recognizing that journaling is often an emotional outlet, the system allows users to categorize their daily state of mind using a primary mood (e.g., Happy, Sad, Anxious) and secondary nuances. This feature transforms the journal from a static repository of text into dynamic emotional data, paving the way for future analytics and insights into the user's well-being over time.

To ensure organization and retrievability, a flexible **Tagging System** has been implemented. Users can tag entries with context (e.g., "Work", "Family", "Travel"), making it effortless to filter and look back on specific life themes later.

Privacy and data ownership are central tenets of the application's design. The system operates entirely offline, storing data locally on the user's machine using a secure SQLite database. This ensures that personal thoughts remain private and clearly under the user's control, without the risks associated with cloud-based storage.

In summary, the Daily Journal application is not merely a text input tool; it is a holistic personal development environment designed to foster consistency, organization, and emotional intelligence through the art of writing.

## b) UI Design (Wireframe)
*(Section omitted as per instructions)*

## c) Data/Entity Modelling

Below are the details required to construct the Entity-Relationship Diagram (ERD).

### 1. Entities & Attributes

**Entity: JournalEntry**
*   **Id** (PK, Integer): Unique identifier for the entry.
*   **EntryDate** (DateTime): The date the entry was written (Unique constraint).
*   **Content** (String): The journal text (Markdown).
*   **PrimaryMood** (String): The main mood selected.
*   **SecondaryMoods** (String): Comma-separated list of additional moods.
*   **Tags** (String): Comma-separated list of tags.
*   **CreatedAt** (DateTime): System timestamp of creation.
*   **UpdatedAt** (DateTime): System timestamp of last update.

**Entity: Mood** (Lookup Table)
*   **Id** (PK, Integer): Unique identifier.
*   **Name** (String): The name of the mood (e.g., "Happy").
*   **Category** (String): Category of the mood (Positive/Neutral/Negative).

**Entity: Tag** (Lookup Table)
*   **Id** (PK, Integer): Unique identifier.
*   **Name** (String): The name of the tag (e.g., "Work").

### 2. Relationships (Logical)

Although the application uses a lightweight SQLite implementation where relationships are denormalized (stored as strings within the `JournalEntry` for simplicity), the **Logical Relationships** for the ERD are:

1.  **JournalEntry** to **Mood** (Many-to-Many)
    *   *Logic*: A Journal Entry can have multiple moods (1 Primary + up to 2 Secondary). A specific Mood (e.g., "Happy") can appear in many different Journal Entries.
2.  **JournalEntry** to **Tag** (Many-to-Many)
    *   *Logic*: A Journal Entry can have multiple tags. A specific Tag (e.g., "Work") can be used in many Journal Entries.

*Note for ERD: You can represent these as Many-to-Many relationships, or note that in the physical implementation, they form a "Loose Relationship" independent of Foreign Keys.*

## d) Technology Stack

The project utilizes a modern Microsoft-centric stack to deliver a high-performance desktop application.

#### i) Framework
*   **Microsoft .NET MAUI Blazor Hybrid (.NET 9.0)**:
    *   **Reason for choice**: This hybrid approach combines the native power of .NET MAUI (Multi-platform App UI) with the flexibility of web technologies (Blazor/HTML/CSS).
    *   **Benefit**: It allows the use of modern HTML5/CSS3 for rendering the complex UI (like the Rich Text inputs and Mood chips) while maintaining full access to native device capabilities and the file system. It targets **Windows (WinUI 3)** for this coursework.

#### ii) External Libraries
*   **sqlite-net-pcl**:
    *   **Purpose**: A lightweight Object-Relational Mapper (ORM) for .NET.
    *   **Usage**: Handles all database interactions, including table creation, querying, and CRUD operations.
*   **SQLitePCLRaw.bundle_green**:
    *   **Purpose**: Provides the underlying native SQLite provider.
    *   **Usage**: Ensures SQLite works correctly across different architectures (Windows, Android, etc.).
*   **Markdig**:
    *   **Purpose**: A fast, powerful Markdown processor for .NET.
    *   **Usage**: Renders the Markdown text from journal entries into HTML for the Preview feature.
*   **Bootstrap**:
    *   **Purpose**: CSS Framework.
    *   **Usage**: Provides the responsive grid system and base styling for the Blazor UI components.

#### iii) Persistence Mechanism
*   **SQLite**:
    *   **Type**: Serverless, local, file-based relational database.
    *   **Storage Location**: The database file (`journal.db3`) is stored in the user's local `AppData` directory (`FileSystem.AppDataDirectory`). This ensures that the data persists across application restarts and is stored in a standard location for user data on Windows.
