import { defineConfig } from 'orval';

export default defineConfig({
    api: {
        input: 'http://localhost:5273/swagger/v1/swagger.json',
        output: {
            target: './src/app/core/api/endpoints.ts',
            schemas: './src/app/core/api/models',
            client: 'angular',
            mode: 'split',
        },
    },
});
