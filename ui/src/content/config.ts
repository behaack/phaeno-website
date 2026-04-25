import { defineCollection, z } from 'astro:content';
import { file } from 'astro/loaders';

const jobs = defineCollection({
  schema: z.object({
    id: z.string(),
    title: z.string(),    
    locationType: z.enum(['Remote','On-Site']),
    locationDescription: z.string(),
    city: z.string().nullable().optional(),
    region: z.string().nullable().optional(),
    country: z.string().nullable().optional(),
    employmentType: z.enum(['Full-time','Part-time','Contract', 'Temporary', 'Intern', 'Other']),
    date: z.coerce.date(),
    summary:  z.string().max(150, 'Maximum length is 150 characters'),
  })
});

const blog = defineCollection({
  schema: z.object({
    title: z.string(),
    tagline: z.string(),
    image: z.string(),
    authors: z.array(z.string()),
    date: z.coerce.date(),
    summary:  z.string(),
  })
});

const events = defineCollection({
  loader: file("src/content/events/events.json"),
  schema: z.object({
    id: z.number(),
    name: z.string(),
    location: z.string(),
    path: z.string(),
    dates: z.string(),
    lastdate: z.coerce.date()
  })
});

const news = defineCollection({
  schema: z.object({
    title: z.string(),
    image: z.string(),
    date: z.coerce.date(),
    summary: z.string(),
  })
});


const press = defineCollection({
  schema: z.object({
    title: z.string(),
    date: z.coerce.date(),
    summary: z.string(),
  })
});

const scientific_papers = defineCollection({
  schema: z.object({
    title: z.string(),
    image: z.string(),
    authors: z.array(z.string()),
    journal: z.string(),
    date: z.coerce.date(),
    link: z.string(),
    summary: z.string(),
  })
});

const white_papers = defineCollection({
  schema: z.object({
    title: z.string(),
    image: z.string(),
    authors: z.array(z.string()),
    date: z.coerce.date(),
    summary: z.string(),
  })
});

export const collections = {
  blog,
  events,
  jobs,
  news,
  press,
  scientific_papers,
  white_papers
};