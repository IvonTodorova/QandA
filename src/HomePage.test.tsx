import React from 'react';
import { render, cleanup, waitFor } from '@testing-library/react';
import { HomePage } from './HomePage';

import { BrowserRouter } from 'react-router-dom';
afterEach(cleanup);
test('When HomePage first rendered, loading indicator should show', () => {
  let mock: any = jest.fn();
  const { getByText } = render(
    <BrowserRouter>
      <HomePage
        history={mock}
        location={mock}
        match={mock}
        getUnansweredQuestions={mock}
        questions={mock}
        questionsLoading={true}
      />
    </BrowserRouter>,
  );
  const loading = getByText('Loading...');
  expect(loading).not.toBeNull();
});

test('When HomePage data returned, it should render questions', async () => {
  let mock: any = jest.fn();
  var questions = [
    {
      questionId: 1,
      title: 'Title1 test',
      content: 'Content2 test',
      userName: 'User1',
      created: new Date(2019, 1, 1),
      answers: [],
    },
    {
      questionId: 2,
      title: 'Title2 test',
      content: 'Content2 test',
      userName: 'User2',
      created: new Date(2019, 1, 1),
      answers: [],
    },
  ];
  const { getByText } = render(
    <BrowserRouter>
      <HomePage
        history={mock}
        questions={questions}
        questionsLoading={false}
        location={mock}
        match={mock}
        getUnansweredQuestions={mock}
      />
    </BrowserRouter>,
  );
  await waitFor(() => getByText('Title1 test'));
  const question2TitleText = getByText('Title2 test');
  expect(question2TitleText).not.toBeNull();
});
