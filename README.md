# Information Search Assistant

In an enterprise world of information it's quite difficult to find
exactly what you are looking for. ISA will try to make this process effortless and faster.

ISA (Information Search Assistant) is an application that understands questions in an arbitrary form and searches the indexed
knowledge base using [elasticsearch](https://www.elastic.co/), which aggregates various sources
of knowledge about the product and processes.

Currently, it supports the following knowledge sources:
* Jira
* Confluence
* Answers of other team members

User interaction in this version is carried out through [slack chatbot](https://slack.com/apps)

## Scheme of work

![alt process](./utils/images/isa_work_process.png)

## Table of contents
* [Getting started](./docs/GETTING_STARTED.md)
* [License](./docs/LICENSE.md)

## FAQ
List of popular questions and answers.
#### How to find the path to the config folder of elasticsearch after installation?
- Run elasticsearch, open in browser `{elasticUrl}/_nodes/settings?pretty=true`, look at **nodes.path.data**.