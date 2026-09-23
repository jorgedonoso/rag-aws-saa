# RAG AWS SAA

A **Retrieval-Augmented Generation (RAG)** API built with **.NET 10, PostgreSQL, pgvector, and OpenAI**.

I chose **RonitSachdev’s AWS SAA-C03 Guides** as the corpus because I found them helpful while preparing for and passing the certification. The guides are licensed under **CC BY 4.0**; see the [original repository](https://github.com/RonitSachdev/aws-saa-c03-guides) and [LICENSE](https://github.com/RonitSachdev/aws-saa-c03-guides/blob/main/LICENSE).

## How It Works

1. **Read** — Load Markdown documents.
2. **Chunk** — Split documents into smaller sections.
3. **Embed** — Generate OpenAI vector embeddings.
4. **Store** — Save chunks and embeddings in PostgreSQL + pgvector.
5. **Retrieve** — Find relevant chunks using vector similarity.
6. **Generate** — Send retrieved context to an LLM.

```mermaid
    flowchart LR
    A[Documents] --> B[Chunks] --> C[Embeddings] --> D[(pgvector)]
    E[Question] --> F[Embedding] --> G[Similarity Search]
    D --> G --> H[LLM] --> I[Answer]

    classDef source fill:#e8f1ff,stroke:#4a78c2,color:#1f2937
    classDef process fill:#eef7ee,stroke:#5b8c5a,color:#1f2937
    classDef store fill:#fff4df,stroke:#c58a2b,color:#1f2937
    classDef output fill:#f3e8ff,stroke:#8b5cf6,color:#1f2937

    class A,E source
    class B,C,F,G process
    class D store
    class H,I output
```

## API

```text
GET /ingest
GET /search?q=...
GET /ask?q=...
```

`/ingest` clears existing chunks and re-ingests the document corpus.

## Environment Variables

```bash
export ConnectionStrings__RagDatabase='...'
export OPENAI_API_KEY='...'
```

## Author

Jorge Donoso
