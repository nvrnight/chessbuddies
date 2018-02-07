alter default privileges in schema public grant all on tables to chessbuddies;

create table public.matches (
    id uuid not null primary key,
    matchjson text not null,
    lastmove timestamp null
);