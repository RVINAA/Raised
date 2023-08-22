*Crappy project to deal with Jenkins CI/CD and some lazy integration tests.. when fail like a shotgun.*

```
curl -X POST -H "Content-Type: application/json" -k https://localhost:5001/api/jenkins/schedules/add -d '{ "repository": "whatever", "branch": "juanje/dummy-raised" }'
```
